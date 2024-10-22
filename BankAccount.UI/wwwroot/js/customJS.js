setTimeout(function () {
    var alert = document.getElementById('alert');
    if (alert) {
        $('#alert').alert('close');

        //alert.style.display = 'none'; // Hides the alert element
        // or
        //alert.remove(); // Removes the alert element from the DOM
    }
}, 5000);// Time in milliseconds before the alert auto-closes (5 seconds here)