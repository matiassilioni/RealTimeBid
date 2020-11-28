# RealTimeBid

 Class responsabilities Description

### AdvertiserRepository
Has DB Cache of all advertisers. Every db call should be done through this class, so cache is maintained in one place
Scaling restriction: Only one node with this service.

### AdvertiserCache
This class receives updated list of Advertisers every period of time (30 secs) where it build an internal list of filtered advertisers by enabled, non exceeding hourly budget nor working date/hour available. 
Maps DbAdvertiser to Advertiser class, which has two hashsets for requrired and rejected tags, optional tags are discarded.

### RtmBidService
Contains a refference to the current node cache (AdvertiserCache). To process a bid, reads the current cache data and puts it into a linked list. With this for every bid, it iterates bid tags (not so much) and traverse the list. If the tag is rejected by the current advertaiser it's easily removed from the list, so it's not checked for the next bid tag. Also if the tag is required by the advertiser and it's in the bid, it's counted.
Once all bid tags were iterated, it traverse the list again until it get the first with all required tags matching (node.MissingRequeriments ==0). Advertiser list in cache are sorted by cpm high to low, so first match in the list is the best bet.

### IPrintNotificationService

implementations of this interface have to push the print to a message queue. Different listeners receive a copy of this message. Each one will do it own work. Microservices for this is the best.
Those could be:
PrintCountService => calls AdvertiserRepository.IncrementPrint(adv, datetime)
LoggerService => Logs every print somewhere


This service scales to many nodes as needed except AdvertiserRepository. Each RTBidService node needs a RtBidService instance and an AdvertiserCache instance. The node is responsable of calling Update to the AdvertiserCache every 30 seconds at least.

Errors
Max budget of an advertiser can be exceded by a small value: all prints in the 30 seconds window that exceed the max budget.
Same for Date/hour restrictions and enabled/disabled advertiser.


