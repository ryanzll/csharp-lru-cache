At present, the capacity of the cache is a simple fixed number.  There are cases in which the size of the cache is dependent on other factors, e.g. the items in the cache use resources from a pool of fixed size, and you want to manage the cache not by the number of items in it but by whether or not there are resources available in the pool.

You'd manage that situation at present by creating an `LRUCache` with an arbitrarily large capacity (it doesn't actually allocate resources based on the capacity) and then manage the cache yourself when creating new items via this pattern:

```
while (!ResourcesAvailable())
{
   cache.Remove(cache.Oldest);
}
MyItem item = new MyItem();
cache.Add(item);
```

Note that if resources need to be freed explicitly and deterministically, this should be done when handling the `RemovingOldestItem` event handler.

It seems to me that I could add a `ResourcesAvailableDelegate` property to the `LRUCache` and add a method to the class that discards the oldest item until there are resources available.  Is that worth doing?  I'm not sure that

```
cache.ResourcesAvailableDelegate += ResourcesAvailable;
```

is a lot more concise than the code shown above, and it also seems to muddy the separation of concerns a bit.  So I probably won't do this unless someone can persuade me it's a good idea.