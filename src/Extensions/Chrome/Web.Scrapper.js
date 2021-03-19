
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
    window.addEventListener("mouseover", onMouseOver);
    window.addEventListener("mouseout", onMouseOut);
    window.addEventListener("mousedown", onMouseDown,true);
    window.addEventListener("click", onClick, true);   
}

function stopListening()
{
    window.removeEventListener("mouseover", onMouseOver);
    window.removeEventListener("mouseout", onMouseOut);
    window.removeEventListener("mousedown", onMouseDown);
    window.removeEventListener("click", onClick);
}


function onMouseOver(event)
{
    $(event.target).css("border", "1px solid red");
}

function onMouseOut(event)
{
    $(event.target).css("border", "");
}

function onMouseDown(event)
{
    if (event.ctrlKey) {
        event.preventDefault();
        event.stopImmediatePropagation()
        $(event.target).css("border", "1px solid orange");
        setTimeout(function () {
            extractDetails(event.target);
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
//BoundingBox.prototype.getBoundingBox = function () {
//    return this.left + '|' + this.top + '|' + this.width + '|' + this.height;
//};


var GetScreenCoordinate = function (elem) {
    var elemBounds = elem.getBoundingClientRect();
    if (window === top) {
        elemBounds = new BoundingBox(elemBounds.left, elemBounds.top, elemBounds.right - elemBounds.left, elemBounds.bottom - elemBounds.top);

    }
    else {
        elemBounds = new BoundingBox(elemBounds.left, elemBounds.top, elemBounds.right - elemBounds.left, elemBounds.bottom - elemBounds.top);
        var currentWindow = window;
        while (true) {
            var frameBounds = currentWindow.frameElement.getBoundingClientRect();
            elemBounds = new BoundingBox(elemBounds.left + frameBounds.left, elemBounds.top + frameBounds.top,
                elemBounds.width, elemBounds.height);
            if (currentWindow.parent === top)
                break;
            currentWindow = currentWindow.parent;
        }
    }
    elemBounds.left += window.top.screenX === 0 ? window.top.screenX : window.top.screenX + 7;
    elemBounds.top += window.top.screenY === 0 ? window.top.screenY : window.screenY - 7;
    elemBounds.top += (window.top.outerHeight - window.top.innerHeight);
    return elemBounds;
};

function frameIdentity(id, name, index)
{
    this.id = id;
    this.name = name;
    this.index = index;
    this.getDetails = function () {
        return this.id + '|' + this.name + '|' + this.index;
    };
}

function extractDetails(control) {
   
    var selector = OptimalSelect.select(control);
    var bounds = GetScreenCoordinate(control);
    var controlDetails = {
        "controlLocation": window.location.hostname, "identifier": selector,  "left": Math.trunc(bounds.left), "top": Math.trunc(bounds.top), "width": Math.trunc(bounds.width), "height": Math.trunc(bounds.height)        
    };
    //console.log(selector + " : " + event.target)
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
                    frameHierarchy.push(new frameIdentity(current.frameElement.id, current.frameElement.name, i).getDetails());
                    break;
                }
            }
            current = parent;
            parent = current.parent;
        }
        while (current !== top);
    }
    controlDetails.frameHierarchy = frameHierarchy.reverse();
    console.log("Selector : " + controlDetails.identifier + " , frame : " + controlDetails.frameIndex);

    //send message after a while so that green border has cleared
    setTimeout(function () {
        chrome.runtime.sendMessage(controlDetails);
    }, 2000);
}

