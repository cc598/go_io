window.onload = function() {
   
};


function SentData() {
    var tel = document.getElementById('tel').value;	
    var regExp = /(1[3|4|5|8][0-9]\d{4,8})$/;

    if (tel != '' && regExp.test(username) == false) {
        var error = document.getElementById('error').value;	
        error.innerHTML = 'tel number error.';
    } else {
            var data = {
	        tel: Tel
	    };

	    $.ajax({
		type: "POST",
		url: "http://localhost:8080/",
		contentType: "application/x-www-form-urlencoded;charset=gb2312",
		data: data,
		success: function(msg) {
		    alert(msg);
		    console.log(msg);
		}
	    });

    }



}

function Reset() {
    var inputElement = document.getElementsByTagName('input');
    inputElement.forEach(function(elem) {
        elem.value = '';
    });
}
