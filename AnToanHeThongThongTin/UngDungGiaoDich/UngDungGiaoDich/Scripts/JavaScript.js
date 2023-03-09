
console.log("aaaa")

window.onload = function () {
    function compare(IdClose, IdCompare) {
        var closeItem = document.getElementById(IdClose);
        var item = document.getElementById(IdCompare);
        if (Number(closeItem.innerText) > Number(item.innerText))
            item.classList.add("text-danger")
        if (Number(closeItem.innerText) < Number(item.innerText))
            item.classList.add("text-success")
        console.log("ok")
    }
}