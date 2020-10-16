(function ($) {
    $(".navbar-form.form-search").submit(function () {
        $('#search-section .container-fluid').css("min-height", '235px');
        $('#search-section .container-fluid').css("padding", '2% 10%');
        $('body,html').animate({ scrollTop: 0 }, 1000);

    });
    $.fn.selectbox = function () {
        $('.dropdown-menu.user-serach li.selectTypeLi').click(function () {
            $("#selectType").html($(this).html() + "<span class='caret'></span>");
            console.log($(this).html());
        });
    };
    $.fn.selectbox = function () {
        $('.dropdown-menu li.selectTypeLi').click(function () {
            $("#selectType").html($(this).html() + "<span class='caret'></span>");
        });
    };
})(jQuery);