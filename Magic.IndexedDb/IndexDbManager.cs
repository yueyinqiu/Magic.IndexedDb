using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Magic.IndexedDb.Factories;
using Magic.IndexedDb.Helpers;
using Magic.IndexedDb.Models;
using Magic.IndexedDb.SchemaAnnotations;
using Microsoft.JSInterop;
using System.Text.Json.Nodes;
using Magic.IndexedDb.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Options;
using Magic.IndexedDb.Extensions;

namespace Magic.IndexedDb
{
    /// <summary>
    /// Provides functionality for accessing IndexedDB from Blazor application
    /// </summary>
    public sealed class IndexedDbManager
    {
        internal static async ValueTask<IndexedDbManager> CreateAndOpenAsync(
            DbStore dbStore, IJSObjectReference jsRuntime,
            CancellationToken cancellationToken = default)
        {
            var result = new IndexedDbManager(dbStore, jsRuntime);
            await result.CallJsAsync(IndexedDbFunctions.CREATE_DB, cancellationToken, new TypedArgument<DbStore>(dbStore));
            return result;
        }

        readonly DbStore _dbStore;
        readonly IJSObjectReference _jsModule;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dbStore"></param>
        /// <param name="jsRuntime"></param>
        private IndexedDbManager(DbStore dbStore, IJSObjectReference jsRuntime)
        {
            this._dbStore = dbStore;
            this._jsModule = jsRuntime;
        }

        // TODO: make it readonly
        public List<StoreSchema> Stores => this._dbStore.StoreSchemas;
        public int CurrentVersion => _dbStore.Version;
        public string DbName => _dbStore.Name;

        /// <summary>
        /// Deletes the database corresponding to the dbName passed in
        /// </summary>
        /// <param name="dbName">The name of database to delete</param>
        /// <returns></returns>
        public Task DeleteDbAsync(string dbName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentException("dbName cannot be null or empty", nameof(dbName));
            }
            return CallJsAsync(IndexedDbFunctions.DELETE_DB, cancellationToken, new TypedArgument<string>(dbName));
        }

        public async Task AddAsync<T>(T record, CancellationToken cancellationToken = default) where T : class
        {
            _ = await AddAsync<T, JsonElement>(record, cancellationToken);
        }

        public async Task<TKey> AddAsync<T, TKey>(T record, CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            StoreRecord<T?> RecordToSend = new StoreRecord<T?>()
            {
                DbName = this.DbName,
                StoreName = schemaName,
                Record = record
            };
            return await CallJsAsync<TKey>(IndexedDbFunctions.ADD_ITEM, cancellationToken, new TypedArgument<StoreRecord<T?>>(RecordToSend));
        }

        [Obsolete]
        public async Task<string> DecryptAsync(
            string EncryptedValue, CancellationToken cancellationToken = default)
        {
            return "Obsolete, decryptions no longer functioning";
            /*EncryptionFactory encryptionFactory = new EncryptionFactory(this);
            string decryptedValue = await encryptionFactory.DecryptAsync(
                EncryptedValue, _dbStore.EncryptionKey, cancellationToken);
            return decryptedValue;*/
        }

        // Returns the default value for the given type
        private static object? GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        //public async Task<Guid> AddRange<T>(IEnumerable<T> records, Action<BlazorDbEvent> action = null) where T : class
        //{
        //    string schemaName = SchemaHelper.GetSchemaName<T>();
        //    var propertyMappings = ManagerHelper.GeneratePropertyMapping<T>();

        //    List<object> processedRecords = new List<object>();
        //    foreach (var record in records)
        //    {
        //        object processedRecord = await ProcessRecord(record);

        //        if (processedRecord is ExpandoObject)
        //        {
        //            var convertedRecord = ((ExpandoObject)processedRecord).ToDictionary(kv => kv.Key, kv => (object)kv.Value);
        //            processedRecords.Add(ManagerHelper.ConvertPropertyNamesUsingMappings(convertedRecord, propertyMappings));
        //        }
        //        else
        //        {
        //            var convertedRecord = ManagerHelper.ConvertRecordToDictionary((T)processedRecord);
        //            processedRecords.Add(ManagerHelper.ConvertPropertyNamesUsingMappings(convertedRecord, propertyMappings));
        //        }
        //    }

        //    return await BulkAddRecord(schemaName, processedRecords, action);
        //}

        /// <summary>
        /// Adds records/objects to the specified store in bulk
        /// Waits for response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recordsToBulkAdd">An instance of StoreRecord that provides the store name and the data to add</param>
        /// <returns></returns>
        private Task BulkAddRecordAsync<T>(
            string storeName,
            IEnumerable<T> recordsToBulkAdd,
            CancellationToken cancellationToken = default)
        {
            // TODO: https://github.com/magiccodingman/Magic.IndexedDb/issues/9

            return CallJsAsync(IndexedDbFunctions.BULKADD_ITEM, cancellationToken,
                new ITypedArgument[] { new TypedArgument<string>(DbName),
                    new TypedArgument<string>(storeName),
                    new TypedArgument<IEnumerable<T>>(recordsToBulkAdd) });
        }

        public async Task AddRangeAsync<T>(
            IEnumerable<T> records, CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            await BulkAddRecordAsync(schemaName, records, cancellationToken);
        }

        public async Task<int> UpdateAsync<T>(T item, CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            object? primaryKeyValue = AttributeHelpers.GetPrimaryKeyValue<T>(item);
            if (primaryKeyValue is null)
                throw new ArgumentException("Item being updated must have a key.");

            UpdateRecord<T> record = new UpdateRecord<T>()
            {
                Key = primaryKeyValue,
                DbName = this.DbName,
                StoreName = schemaName,
                Record = item
            };

            return await CallJsAsync<int>(IndexedDbFunctions.UPDATE_ITEM, cancellationToken, new TypedArgument<UpdateRecord<T?>>(record));
        }

        public async Task<int> UpdateRangeAsync<T>(
    IEnumerable<T> items,
    CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            var recordsToUpdate = items.Select(item =>
            {
                object? primaryKeyValue = AttributeHelpers.GetPrimaryKeyValue<T>(item);
                if (primaryKeyValue is null)
                    throw new ArgumentException("Item being updated must have a key.");

                return new UpdateRecord<T>()
                {
                    Key = primaryKeyValue,
                    DbName = this.DbName,
                    StoreName = schemaName,
                    Record = item
                };
            });

            return await CallJsAsync<int>(
                IndexedDbFunctions.BULKADD_UPDATE, cancellationToken, new TypedArgument<IEnumerable<UpdateRecord<T>>>(recordsToUpdate));
        }


        public async Task<T?> GetByIdAsync<T>(
            object key,
            CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            // Validate key type
            AttributeHelpers.ValidatePrimaryKey<T>(key);

            return await CallJsAsync<T>(
                IndexedDbFunctions.FIND_ITEM, cancellationToken,
                new ITypedArgument[] { new TypedArgument<string>(DbName), new TypedArgument<string>(schemaName), new TypedArgument<object>(key) });
        }

        public MagicQuery<T> Query<T>() where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();
            MagicQuery<T> query = new MagicQuery<T>(schemaName, this);
            return query;
        }

        internal async Task<IEnumerable<T>?> WhereV2Async<T>(
            string storeName, List<string> jsonQuery, MagicQuery<T> query,
            CancellationToken cancellationToken) where T : class
        {
            string? jsonQueryAdditions = null;
            if (query != null && query.StoredMagicQueries != null && query.StoredMagicQueries.Count > 0)
            {
                jsonQueryAdditions = MagicSerializationHelper.SerializeObject(query.StoredMagicQueries.ToArray());
            }

            var args = new ITypedArgument[] {
                new TypedArgument<string>(DbName),
                new TypedArgument<string>(storeName),
                new TypedArgument<string[]>(jsonQuery.ToArray()),
                new TypedArgument<string>(jsonQueryAdditions!),
                new TypedArgument<bool?>(query?.ResultsUnique!),
            };

            return await CallJsAsync<IEnumerable<T>>
                (IndexedDbFunctions.WHERE, cancellationToken,
                args);
        }

       

        private object ConvertValueToType(object value, Type targetType)
        {
            if (targetType == typeof(Guid) && value is string stringValue)
            {
                return Guid.Parse(stringValue);
            }
            if (targetType.IsEnum)
            {
                return Enum.ToObject(targetType, Convert.ToInt64(value));
            }

            var nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
            {
                // It's nullable
                if (value == null)
                    return null;

                return Convert.ChangeType(value, nullableType);
            }
            return Convert.ChangeType(value, targetType);
        }

        

        /// <summary>
        /// Returns Mb
        /// </summary>
        /// <returns></returns>
        public Task<QuotaUsage> GetStorageEstimateAsync(CancellationToken cancellationToken = default)
        {
            return CallJsAsync<QuotaUsage>(IndexedDbFunctions.GET_STORAGE_ESTIMATE, cancellationToken, []);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();
            return await CallJsAsync<IList<T>>(
                IndexedDbFunctions.TOARRAY, cancellationToken,
                new ITypedArgument[] { new TypedArgument<string>(DbName), new TypedArgument<string>(schemaName) });
        }

        public async Task DeleteAsync<T>(T item, CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            object? primaryKeyValue = AttributeHelpers.GetPrimaryKeyValue<T>(item);

            UpdateRecord<T> record = new UpdateRecord<T>()
            {
                Key = primaryKeyValue,
                DbName = this.DbName,
                StoreName = schemaName,
                Record = item
            };

            await CallJsAsync(IndexedDbFunctions.DELETE_ITEM, cancellationToken, new TypedArgument<UpdateRecord<T?>>(record));
        }

        public async Task<int> DeleteRangeAsync<T>(
    IEnumerable<T> items, CancellationToken cancellationToken = default) where T : class
        {
            string schemaName = SchemaHelper.GetSchemaName<T>();

            var keys = items.Select(item =>
            {
                object? primaryKeyValue = AttributeHelpers.GetPrimaryKeyValue(item);
                if (primaryKeyValue is null)
                    throw new ArgumentException("Item being deleted must have a key.");
                return primaryKeyValue;
            });

            var args = new ITypedArgument[] {
                new TypedArgument<string>(DbName),
                new TypedArgument<string>(schemaName),
                new TypedArgument<IEnumerable<object>?>(keys) };

            return await CallJsAsync<int>(
                IndexedDbFunctions.BULK_DELETE, cancellationToken,
                args);
        }


        /// <summary>
        /// Clears all data from a Table but keeps the table
        /// Wait for response
        /// </summary>
        /// <param name="storeName"></param>
        /// <returns></returns>
        public Task ClearTableAsync(string storeName, CancellationToken cancellationToken = default)
        {
            return CallJsAsync(IndexedDbFunctions.CLEAR_TABLE, cancellationToken,
                new ITypedArgument[] { new TypedArgument<string>(DbName), new TypedArgument<string>(storeName) });
        }

        /// <summary>
        /// Clears all data from a Table but keeps the table
        /// Wait for response
        /// </summary>
        /// <returns></returns>
        public Task ClearTableAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            return ClearTableAsync(SchemaHelper.GetSchemaName<T>(), cancellationToken);
        }

        internal async Task CallJsAsync(string functionName, CancellationToken token, params ITypedArgument[] args)
        {
            var magicJsInvoke = new MagicJsInvoke(_jsModule);

            await magicJsInvoke.MagicVoidStreamJsAsync(functionName, token, args);
        }

        internal async Task<T> CallJsAsync<T>(string functionName, CancellationToken token, params ITypedArgument[] args)
        {

            var magicJsInvoke = new MagicJsInvoke(_jsModule);

            return await magicJsInvoke.MagicStreamJsAsync<T>(functionName, token, args) ?? default;
        }
    }
}