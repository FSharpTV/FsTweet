$(function(){
   $.getJSON("/wall", function(){

    }).done(function(data){
      console.log(data);
    }).fail(function(x){
      console.log(x);
      alert('error');
    });
});