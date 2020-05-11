$(document).ready(function () {

    var load = window.addEventListener("load", function () { loadImages(); });
    var resize = window.addEventListener("resize", function () { loadImages(); });
    var scroll = window.addEventListener("scroll", function () { loadImages(); });

    function loadImages() {
        var images = document.querySelectorAll(".lazy-load");
        for (var i = 0; i < images.length; i++) {
            var imageBounds = images[i].getBoundingClientRect();
            if (imageBounds.top >= 0 &&
                imageBounds.left >= 0 &&
                imageBounds.bottom <= window.innerHeight &&
                imageBounds.right <= window.innerWidth) {
                images[i].src = '/' + images[i].dataset.src;
            }
        }
    }

    "use strict"; $('.menu > ul > li:has( > ul)').addClass('menu-dropdown-icon'); $('.menu > ul > li > ul:not(:has(ul))').addClass('normal-sub'); $(".menu > ul > li").hover(function (e) { if ($(window).width() > 943) { $(this).children("ul").stop(true, false).fadeToggle(150); e.preventDefault(); } }); $(".menu > ul > li").on('click', function () { if ($(window).width() <= 943) { $(this).children("ul").fadeToggle(150); } }); $(".h-bars").on('click', function (e) { $(".menu > ul").toggleClass('show-on-mobile'); e.preventDefault(); });
});