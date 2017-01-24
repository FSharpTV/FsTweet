$(function(){
  var $tweetForm = $(".form-tweet")
  $tweetForm.submit(function(e){
    $.ajax({
      type: $tweetForm.attr('method'),
      url: $tweetForm.attr('action'),
      data: $tweetForm.serialize(),
      success: function (data) {
          alert('success');
          $("#tweet").val('');
      },
      error: function(data) {
        alert(data.responseText)
      }
    });
    e.preventDefault()
  });
})