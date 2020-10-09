(function ($) {
    //$(".hrefs-query a").click(function () {
    //    $("#input-string-query").val(this.innerHTML);
    //    $(".navbar-form.form-search").submit();
    //    return false;
    //});



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

        //// начальные параметры
        //// задаем стандартную высоту div'a. 
        //var selectDefaultHeight = $('#selectBox').height();
        //// угол поворота изображения в div'e 
        //var rotateDefault = "rotate(0deg)";

        //// после нажатия кнопки срабатывает функция, в которой 
        //// вычисляется исходная высота нашего div'a. 
        //// очень удобно для сравнения с входящими параметрами (то, что задается в начале скрипта) 
        //$('#selectBox > p.valueTag').click(function () {
        //    // вычисление высоты объекта методом height() 
        //    var currentHeight = $('#selectBox').height();
        //    // проверка условия на совпадение/не совпадение с заданной высотой вначале,
        //    // чтобы понять. что делать дальше. 
        //    if (currentHeight < 100 || currentHeight == selectDefaultHeight) {
        //        // если высота блока не менялась и равна высоте, заданной по умолчанию,
        //        // тогда мы открываем список и выбираем нужный элемент.
        //        $('#selectBox').height("180px");  // «точка остановки анимации»
        //        // здесь стилизуем нашу стрелку и делаем анимацию средствами CSS3 
        //        $('i.arrow.fa.fa-caret-down').css({ borderRadius: "1000px", transition: ".2s", transform: "rotate(180deg)" });
        //    }


        //    // иначе если список развернут (высота больше или равна 250 пикселям), 
        //    // то при нажатии на абзац с классом valueTag, сворачиваем наш список и
        //    // и присваиваем блоку первоначальную высоту + поворот стрелки в начальное положение
        //    if (currentHeight >= 250) {
        //        $('#selectBox').height(selectDefaultHeight);
        //        $('i.arrow.fa.fa-caret-down').css({ transform: rotateDefault });
        //    }
        //});

        //// так же сворачиваем список при выборе нужного элемента 
        //// и меняем текст абзаца на текст элемента в списке
        //$('li.option').click(function () {
        //    $('#selectBox').height(selectDefaultHeight);
        //    $('img.arrow').css({ transform: rotateDefault });
        //    $('#asdfasdf').html($(this).html());
        //});
    };
})(jQuery);