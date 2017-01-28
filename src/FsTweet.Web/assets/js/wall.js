$(function(){
   $.getJSON("/wall", function(){

    }).done(function(data){
      window.renderTweets(data, $("#wall"))
    }).fail(function(x){
      console.log(x);
      alert('error');
    });
});