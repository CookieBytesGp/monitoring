// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-hide alerts after 5 seconds
$(document).ready(function() {
    // Auto-hide success alerts after 5 seconds
    $('.alert-success').each(function() {
        var alert = $(this);
        setTimeout(function() {
            alert.fadeOut('slow');
        }, 5000);
    });
    
    // Auto-hide info alerts after 7 seconds
    $('.alert-info').each(function() {
        var alert = $(this);
        setTimeout(function() {
            alert.fadeOut('slow');
        }, 7000);
    });
    
    // Warning and error alerts remain until manually dismissed
});

// Add smooth transitions for alerts
$('.alert').on('closed.bs.alert', function () {
    $(this).slideUp();
});

// Show loading spinner on form submit
$('form').on('submit', function() {
    var submitBtn = $(this).find('button[type="submit"]');
    if (submitBtn.length > 0) {
        submitBtn.prop('disabled', true);
        var originalText = submitBtn.html();
        submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...');
        
        // Re-enable button after 10 seconds as fallback
        setTimeout(function() {
            submitBtn.prop('disabled', false);
            submitBtn.html(originalText);
        }, 10000);
    }
});
