
chrome.runtime.onInstalled.addListener(() => {
  
});

  
var isExtractionActive = false;

var startScrapping = function ()
{   
    console.log("start scrapping.");    
    isExtractionActive = true;
    chrome.tabs.query({},function (tabs) {
        tabs.forEach(function (tab) {
            if (tab.id > 0) {                     
                 chrome.tabs.sendMessage(tab.id, true, function (response) {
                });
               }
            });
        });       
};

var stopScrapping = function() {
    console.log("stop scrapping.");    
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
chrome.action.onClicked.addListener(function (tab) {

    if (!isExtractionActive) {
        try {
            startScrapping();
        }
        catch (err) {
            console.log(err);
        }
    }
    else {
        try {
            stopScrapping();

        }
        catch (err) {
            console.log(err);
        }

    }
});


chrome.tabs.onCreated.addListener(function (tab) {
    if (!isExtractionActive)
        return;
    chrome.tabs.sendMessage(tab.id, isExtractionActive, function (response) {
    });
});

chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    if (!isExtractionActive)
        return;
    chrome.tabs.sendMessage(tabId, isExtractionActive, function (response) {
    });
});

