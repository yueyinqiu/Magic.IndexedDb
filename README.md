# True LINQ to IndexedDB - Magic IndexedDb
This project is currently under a massive refactor that has successfully built the first true LINQ to IndexedDB translation layer. Currently the released version is `v1.0.12` which is the last update to the version 1.0 era. Documentation is now being updated to reflect the very soon release of version 2.0 but until then, if you see this message. Then version 1.0 is still the supported release but [click here to go to the legacy documentation for `v1.0.12`](https://github.com/magiccodingman/Magic.IndexedDb/blob/master/MagicIndexDbWiki/Version-1.0-Legacy.md).

Note the migration system is under construction. When this is finished, all version 1.0 documentation will cease to exist anywhere but in the legacy documentation area. Version 1.0 will become completely unsupported. 

# **Introduction to LINQ to IndexedDB – The Revolution Begins**

## **What is Magic IndexedDB?**

Welcome to the first-ever **true** LINQ to IndexedDB system. This project is not just another wrapper around IndexedDB—it is a **complete transformation** of how we interact with browser databases, unlocking **seamless, optimized querying** that **feels like LINQ to SQL but is built for IndexedDB**.

At its core, this system allows **C# developers in Blazor** to write LINQ queries that are automatically translated into **the most efficient IndexedDB queries possible**. But beyond that, we have designed a **universal layer** that any programming framework can wrap around, enabling a truly **universal LINQ to IndexedDB library for any language**.

## **What Makes This Different?**

Many past attempts at “LINQ to IndexedDB” were **not** actually LINQ to IndexedDB—they were **just LINQ-like syntax calling IndexedDB’s APIs directly**. These libraries still loaded massive amounts of data into memory before filtering it, **completely missing the point** of LINQ’s efficiency.

### **A Real LINQ to IndexedDB System**
To understand what makes **Magic IndexedDB** revolutionary, we need to compare it to **LINQ in memory vs. LINQ to SQL**:
- **Traditional LINQ in memory** translates your intent into operations like loops and local data grabs.
- **LINQ to SQL** does not load everything into memory first; it **translates your query into the most optimized SQL command possible** before execution.

**Magic IndexedDB does the same for IndexedDB**. Instead of forcing you to deal with raw IndexedDB API calls or load unnecessary data into memory, **we translate your LINQ expressions into efficient IndexedDB queries**.

### **How It Works**
This system **does not** blindly fetch all objects into memory before filtering. Instead:
1. **Expression Parsing** – We take your LINQ expression (your **intent**) and break it down.
2. **Query Optimization** – We analyze your filters and **categorize them into three optimized query types**:
   - **Indexed Queries** – Directly use IndexedDB indexes for hyper-efficient retrieval.
   - **Compound Indexed Queries** – Combine multiple indexed searches where possible.
   - **Cursor-Based Queries** – When indexing isn’t possible, we perform a **single metadata retrieval pass** before pulling in any full objects.
3. **Multi-Query Execution** – We intelligently distribute your conditions across multiple targeted queries **without breaking intent**.
4. **Efficient Memory Handling** – Data is **only pulled into memory when we know exactly what we need**.

This means:
- **You get true LINQ to IndexedDB behavior.**
- **Your queries are optimized at every level.**
- **You never have to manually handle IndexedDB’s quirks again.**

## **Breaking the Limitations of IndexedDB**
One of the biggest challenges in IndexedDB is its **lack of native support for complex `||` (OR) conditions**.  
**Magic IndexedDB** completely **solves this** by:
- **Breaking down complex expressions into multiple optimized queries**.
- **Automatically flattening and restructuring nested conditions** while keeping intent intact.
- **Processing only metadata before pulling full objects into memory**.

### **The Power of the Cursor Meta-Data Algorithm**
For queries that cannot be fully indexed, **we do something unprecedented**:
1. **Meta-Data Pass** – Instead of loading full objects into memory, we first retrieve **only necessary metadata**.
2. **Intelligent Sorting & Filtering** – The metadata is structured **as if it were still in IndexedDB**.
3. **Final Data Retrieval** – Only **after** filtering and sorting, do we fetch the actual objects—**in the exact order required**.

This means even **non-indexed queries are optimized** to prevent unnecessary data loading. **Skip, take, ordering, and nested conditions are handled seamlessly**.

## **What This Means for You**
With **Magic IndexedDB**, working with IndexedDB is no longer a headache:
- **Your queries feel like LINQ to SQL**—no need to think about IndexedDB’s limitations.
- **Your logic stays seamless**—you don’t need to write separate code for indexed vs. non-indexed queries.
- **Your migrations are automated**—upgrading schema versions will be effortless.
- **You get full power and flexibility**—without worrying about performance bottlenecks.


## Self Validation
Additionally this library has self validation which guides you the best it can to build optimized queries. The library will also prevent compilation when you accidentally try to build a schema that's not authorized by IndexedDB.

However, just as **LINQ to SQL is not identical to in-memory LINQ**, **LINQ to IndexedDB also has nuances**. It’s important to understand how your queries are translated and where indexes vs. cursors will be used. Understanding how LINQ to IndexedDB works is important to building optimized queries. Just like in LINQ to SQL where you can build queries accidentally that are not performant, the same idea stands true here!

It's important that you read the LINQ to IndexedDB documentation to truly understand what's going on so you can use IndexedDB like a pro:
## [Click Here to Get Started - Magic IndexedDB Documentation](https://github.com/magiccodingman/Magic.IndexedDb/blob/master/MagicIndexDbWiki/Index.md)

# **Welcome to the Future**
**IndexedDB is no longer a painful, complex system**. With **Magic IndexedDB**, your intent is effortlessly transformed into optimized queries. 

This is **the first and only true LINQ to IndexedDB implementation**—not a fake LINQ-like wrapper, not a memory-hogging abstraction—**a real LINQ system that truly understands IndexedDB**.

Welcome to **Magic IndexedDB**.  
Where **everything is truly magic**.
