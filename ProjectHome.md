This is a collection class that functions as a least-recently-used cache.  It implements `ICollection<T>`, but also exposes three other members:

  * `Capacity`, the maximum number of items the cache can contain.  Once the collection is at capacity, adding a new item to the cache will cause the least recently used item to be discarded.  If the Capacity is set to 0 at construction, the cache will not automatically discard items.

  * `Oldest`, the oldest (i.e. least recently used) item in the collection.

  * `DiscardingOldestItem`, an event raised when the cache is about to discard its oldest item.

This is an extremely simple implementation.  While its Add and Remove methods are thread-safe, it shouldn't be used in heavy multithreading environments because the entire collection is locked during those methods.