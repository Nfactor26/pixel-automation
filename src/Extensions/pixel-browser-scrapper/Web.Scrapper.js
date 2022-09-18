
chrome.runtime.onMessage.addListener(function (recordingState) {

    if (!recordingState)
    {
        console.log("stop listening for scraping events");
        stopListening();
    }
    else
    {
        console.log("start listening for scraping events");
        startListening();
    }   
});


function startListening()
{
    //window.addEventListener("mouseover", onMouseOver, false);
    //window.addEventListener("mouseout", onMouseOut, false);
    //window.addEventListener("mousedown", onMouseDown,false);
    //window.addEventListener("click", onClick, false);

    document.querySelectorAll("body *").forEach(item => {

        if (item.shadowRoot != null) {       
            attachEventsToShadowRootForElementRecursively(item);
        }

        item.addEventListener('mouseover', onMouseOver, false);
        item.addEventListener('mouseout', onMouseOut, false);
        item.addEventListener('mousedown', onMouseDown, false);
        item.addEventListener("click", onClick, false);       
    });
    window.playwright = new InjectedScript(true, 1, "chrome", []);
    console.log("startListening"); // Scrapper plugin looks for this console message to attach to event handler
}

function stopListening()
{
    //window.removeEventListener("mouseover", onMouseOver);
    //window.removeEventListener("mouseout", onMouseOut);
    //window.removeEventListener("mousedown", onMouseDown);
    //window.removeEventListener("click", onClick);

    document.querySelectorAll("*").forEach(item => {

        if (item.shadowRoot != null) {
            removeEventsFromShadowRootForElementRecursively(item);
        }

        item.removeEventListener("mouseover", onMouseOver);
        item.removeEventListener("mouseout", onMouseOut);
        item.removeEventListener("mousedown", onMouseDown);
        item.removeEventListener("click", onClick);
    });
    window.playwright = null;
    console.log("stopListening");
}


function onMouseOver(event)
{     
    $(event.target).css("border", "1px solid red");
}

function onMouseOut(event)
{
    $(event.target).css("border", "");
}


function attachEventsToShadowRootForElementRecursively(element) {

    if (element.shadowRoot === null)
        return;

    element.shadowRoot.querySelectorAll("*").forEach(item => {
        item.addEventListener('mouseover', onMouseOver, false);
        item.addEventListener('mouseout', onMouseOut, false);
        item.addEventListener('mousedown', onMouseDown, false);
        item.addEventListener("click", onClick, false);

        if (item.shadowRoot != null) {
            item.shadowRoot.querySelectorAll("*").forEach(item => {
                if (item.shadowRoot != null)                    
                    attachEventsToShadowRootForElementRecursively(item);
            });
        } 

    });   
}

function removeEventsFromShadowRootForElementRecursively(element) {

    if (element.shadowRoot === null)
        return;

    element.shadowRoot.querySelectorAll("*").forEach(item => {
        item.removeEventListener("mouseover", onMouseOver);
        item.removeEventListener("mouseout", onMouseOut);
        item.removeEventListener("mousedown", onMouseDown);
        item.removeEventListener("click", onClick);

        if (item.shadowRoot != null) {
            item.shadowRoot.querySelectorAll("*").forEach(item => {
                if (item.shadowRoot != null)                   
                    removeEventsFromShadowRootForElementRecursively(item);
            });
        }
    });
}

function onMouseDown(event)
{       
    if (event.ctrlKey) {
        event.preventDefault();
        event.stopImmediatePropagation()
        $(event.target).css("border", "1px solid orange");
        setTimeout(function () {

            if (event.composed && event.composedPaths) {    
                
                extractDetails(event.composedPaths()[0]);
            }
            else if (event.composed && event.path) {     
                
                extractDetails(event.path[0]);
            }
            else {

                extractDetails(event.target);
            }                     
            $(event.target).css("border", "1px solid green");
            event.preventDefault();
        }, 500);
        setTimeout(function () {           
            $(event.target).css("border", "");           
        }, 500);
    }
}

function onClick(event)
{
    if (event.ctrlKey)
    {
        event.preventDefault();      
        event.stopImmediatePropagation();
    }
}

var BoundingBox = function (left, top, width, height) {
    this.left = left;
    this.top = top;
    this.width = width;
    this.height = height;
};

var GetScreenCoordinate = function (elem) {
    var elemBounds = elem.getBoundingClientRect();
    if (window === top) {

        elemBounds = new BoundingBox(elemBounds.left, elemBounds.top, elemBounds.width, elemBounds.height);

    }
    else {
        elemBounds = new BoundingBox(elemBounds.left, elemBounds.top, elemBounds.width, elemBounds.height);
        var currentWindow = window;
        while (true) {
            if (currentWindow.frameElement === null ) {              
                break;
            }
            
            var frameBounds = currentWindow.frameElement.getBoundingClientRect();
            elemBounds = new BoundingBox(elemBounds.left + frameBounds.left, elemBounds.top + frameBounds.top,
                elemBounds.width, elemBounds.height);
            if (currentWindow.parent === top)
                break;
            currentWindow = currentWindow.parent;
        }
    }
    try {
        elemBounds.left += window.top.screenX === 0 ? window.top.screenX : window.top.screenX + 7;
        elemBounds.top += window.top.screenY === 0 ? window.top.screenY : window.screenY - 7;
        elemBounds.top += (window.top.outerHeight - window.top.innerHeight);
    }
    catch(ex) {
        console.log(ex);
    }
    return elemBounds;
};

function frameIdentity(id, name, index, playwrightSelector)
{
    this.id = id;
    this.name = name;
    this.index = index;
    this.playwrightSelector = playwrightSelector;
    this.getDetails = function () {
        return this.id + '|' + this.name + '|' + this.index + '|' + this.playwrightSelector;
    };
}

function extractDetails(control) {

    var playwrightSelector = window.playwright.generateSelector(control);
    
    var optimalSelector = '';
    try
    {
       //The library has issues working with shadow dom elements as getElementsByTagName is not defined inside shdadow dom
       optimalSelector = OptimalSelect.select(control);    
    }
    catch(ex)
    {
        console.log(ex);
    }
   
    var bounds = GetScreenCoordinate(control);
    var controlDetails = {
        "controlLocation": window.location.hostname, "selector": optimalSelector, "playwrightSelector": playwrightSelector,
        "left": Math.trunc(bounds.left), "top": Math.trunc(bounds.top), "width": Math.trunc(bounds.width), "height": Math.trunc(bounds.height)        
    };
    var frameHierarchy = [];
    if (window === top) {
        //frameDetails.push(frameIdentity("","",-1));
    }
    else
    {
        var parent = window.parent;
        var current = window;
        do {
            for (var i = 0; i < parent.frames.length; i++) {
                if (parent.frames[i] === current) {
                    frameHierarchy.push(new frameIdentity(current.frameElement.id, current.frameElement.name, i, window.playwright.generateSelector(control)).getDetails());
                    break;
                }
            }
            current = parent;
            parent = current.parent;
        }
        while (current !== top);
    }
    controlDetails.frameHierarchy = frameHierarchy.reverse();    

    //our browser scrapper plugin will read these details printed on console
    console.log(JSON.stringify(controlDetails));
}
