var interval;

function scrollMoviesRight(id) {
    let scrollerId = 'scroller-' + id;
    interval = setInterval(function () { document.getElementById(scrollerId).scrollLeft += 25 }, 5);
};
function scrollMoviesLeft(id) {
    let scrollerId = 'scroller-' + id;
    interval = setInterval(function () { document.getElementById(scrollerId).scrollLeft -= 25 }, 5);
};
function clearScroll() {
    clearInterval(interval);
};