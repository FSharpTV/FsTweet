$(function(){
   let loadWall = function() {
     $.getJSON("/wall", function(){

    }).done(function(data){
      window.renderTweets(data, $("#wall"))
    }).fail(function(x){
      console.log(x);
      alert('error');
    });
   }
   loadWall()
   window.setInterval(loadWall, 1000)
});