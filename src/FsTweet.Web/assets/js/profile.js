$(function(){

  function loadTweets(username) {
    $.getJSON("/tweets/" + username, function(){

    }).done(function(data){
      console.log(data)
    }).fail(function(data){
      console.log(data)
    });
  }

  loadTweets(window.location.pathname.substring(1))

});