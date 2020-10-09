{
    $(document).ready(function () {
        $('selector').selectbox();
    });
    $(".aLike").click(function (e) {
        if (this.innerHTML.indexOf("fa-heart-o")+1) {
            $id = this.id;
            var data = new FormData();
            data.append("id", $id);
            $.ajax({
                type: "POST",
                url: '/Home/Like',
                contentType: false,
                processData: false,
                data: data,
                success: function (result) {
                    if (result == "Yes") document.getElementById($id).innerHTML = '<i class="fa fa-heart" style="color: #428bca;cursor:auto"></i>';
                },
                error: function (xhr, status, p3) {
                }
            });
        };
        return false;
    });
}