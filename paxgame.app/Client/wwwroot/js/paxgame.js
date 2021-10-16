
function toggleAnimation(run) {
    var units = document.getElementsByClassName("unitanimation");
    for (var i = 0; i < units.length; i = i + 1) {
        units[i].style.animationPlayState = run ? 'running' : 'paused';
    }
};

function moveAnimation(direction) {
    var units = document.getElementsByClassName("unitanimation");

    if (!direction) {
        for (var i = 0; i < units.length; i = i + 1) {
             units[i].style.animationDirection = "reverse";
        }
    }
    
    toggleAnimation(true);
    setTimeout(function () {
        toggleAnimation(false);
    }, 100);

    if (!direction) {
        for (var i = 0; i < units.length; i = i + 1) {
            units[i].style.animationDirection = "forwards";
        }
    }
};

function stepAnimation(direction) {
    var units = document.getElementsByClassName("unitanimation");
    for (var i = 0; i < units.length; i = i + 1) {
        units[i].style.animationDirection = "reverse";
    }
}

function animateElement(_el, _start, _stop, _time) {

    // get the latest position from the computed css 
    if (_el.animateData && _el.animateData.playState !== "finished")
        _el.animateData.reverse()

    // set animation if there's no other current animations in progress
    if (!_el.animateData
        || _el.animateData.playState === "finished")
        _el.animateData = _el.animate([
            { transform: 'translate(' + _start + 'px)' },
            { transform: 'translate(' + _stop + 'px)' }
        ], _time);

    // block the element transition to the '_stop' position
    _el.animateData.onfinish = function () {
        _el.style.transform = 'translate(' + _stop + 'px)';
    };
}


function getBuildSize(elementid, dotnetref) {
    dotnetRef = dotnetref;
    elementId = elementid;
    var ele = document.getElementById(elementid);
    return ele.getBoundingClientRect();
}

function buildResize() {
    if (dotnetRef != null) {
        var ele = document.getElementById(elementId);
        return dotnetRef.invokeMethodAsync("GetResizeInfo", ele.getBoundingClientRect());
    }
}

function throttle(callback, interval) {
    let enableCall = true;

    return function (...args) {
        if (!enableCall) return;

        enableCall = false;
        callback.apply(this, args);
        setTimeout(() => enableCall = true, interval);
    }
}

function dotnetDispose() {
    dotnetRef = null;
    elementId = null;
}

var elementId = null;
var dotnetRef = null;
window.onresize = throttle(buildResize, 200);