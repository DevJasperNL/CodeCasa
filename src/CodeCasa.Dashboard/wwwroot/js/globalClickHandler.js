window.globalClickHandler = {
    initialize: function (dotNetRef) {
        document.addEventListener("click", function (e) {
            dotNetRef.invokeMethodAsync("OnGlobalClick");
        });
    }
};