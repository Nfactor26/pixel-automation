var ah = jQuery.noConflict();
var isExtractionActive = false;
var isConnected = false;

var startConnection = function ()
{
    ah.connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:9143/scrapper")
    .configureLogging(signalR.LogLevel.Information)
    .build();
  
    /*ah.connection.on("StopScraping", () => {
        console.log("Received message to stop scraping.");
        stopScrapping();
    });*/

    ah.connection.start().then(function () {
        console.log("Connected to SignalR server.");        
        isConnected = true;
        isExtractionActive = true;
        chrome.tabs.query({},function (tabs) {
            tabs.forEach(function (tab) {
                if (tab.id > 0) {                     
                     chrome.tabs.sendMessage(tab.id, true, function (response) {
                    });
                   }
                });
            })
        })
        .catch(err => console.error(err));
     
    ah.connection.onclose(function () {
        console.log("Disconnected from scrapper hub.");
        stopScrapping();
    });
};

var stopConnection = function () {
   console.log("Disonnect from server.")
   ah.connection.stop();
   stopScrapping();
};

var stopScrapping = function() {
    console.log("Stop scrapping.");    
    isConnected = false;
    isExtractionActive = false;
    chrome.tabs.query({},function (tabs) {
        tabs.forEach(function (tab) {
            if (tab.id > 0) {
                chrome.tabs.sendMessage(tab.id, false, function (response) {
                });
            }
        });
    });
};

/*This method will be called when the extension icon is clicked in the browser */
chrome.browserAction.onClicked.addListener(function (tab) {

    if (!isExtractionActive) {
        try {
            startConnection();
        }
        catch (err) {
            console.log(err);
        }
    }
    else {
        try {
            stopConnection();

        }
        catch (err) {
            console.log(err);
        }

    }
});


chrome.tabs.onCreated.addListener(function (tab) {
    if (!isConnected)
        return;
    chrome.tabs.sendMessage(tab.id, isExtractionActive, function (response) {
    });
});

chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    if (!isConnected)
        return;
    chrome.tabs.sendMessage(tabId, isExtractionActive, function (response) {
    });
});

chrome.runtime.onMessage.addListener(function (message, sender) {    
    if (isConnected)
    {
        console.log("Selector : " + message.identifier + " , frame : " + message.frameIndex);
        ah.connection.invoke("AddWebControlDetails", message).catch(err => console.error(err));
    }
});


