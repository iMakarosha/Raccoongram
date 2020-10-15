{
    $(document).ready(function () {
        $('selector').selectbox();
    });
    $(".aLike").click(function (e) {
        if (this.innerHTML.indexOf("fa-heart-o") + 1 || this.classList.contains("fa-heart-o")) {
            $id = this.id;
            var data = new FormData();
            data.append("id", $id);
            $.ajax({
                type: "POST",
                url: '/Home/Like',
                cache: false,
                dataType: 'json',
                processData: false,
                contentType: false,
                data: data,
                success: function (result) {
                    if (result == "Yes") document.getElementById($id).innerHTML = '<i class="fa fa-heart" style="color: #900;cursor:auto"></i>';
                },
                error: function (xhr, status, p3) {
                }
            });
            
        };
      
        e.preventDefault();
    });
}