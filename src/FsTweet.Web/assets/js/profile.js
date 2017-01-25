$(function(){

  function loadTweets(username) {
    $.getJSON("/tweets/" + username, function(){

    }).done(function(data){
      var template = $("#tweetReadTmpl").html()
      Mustache.parse(template); 
      var htmlOutput = Mustache.render(template, data);
      $("#tweets").html(htmlOutput)
    }).fail(function(data){
      alert(data)
    });
  }

  loadTweets(window.location.pathname.substring(1))

});