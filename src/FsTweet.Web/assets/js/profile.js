$(function(){  

  var usernameList = `
    {{#usernames}}
      <div class="well">
        <a href="/{{value}}">@{{value}}</a>
      </div>
    {{/usernames}}
  `  

  var username = window.location.pathname.substring(1) 

  function loadTweets(username) {
    $.getJSON("/tweets/" + username, function(){

    }).done(function(data){
      renderTweets(data, $("#tweets"))
    }).fail(function(data){
      alert(data)
    });
  }

  loadTweets(username)

  $(document).on('tweet:posted', function(){
    loadTweets(username)
  })
  
  $("#follow").on('click', function(e){
    $(this).addClass("disabled")
    $.post('/follow', {"username" : username})
      .done(function(){        
        window.location.reload()
      })
      .fail(function(x){
        console.log(x)
        alert('error')
        $(this).removeClass('disabled')
      })
    e.preventDefault()
  });

  function renderUsernames (url, viewId, countId) {
     $.getJSON(url + "/" + username, function() {})
      .done(function(data){
        var htmlOutput = Mustache.render(usernameList, {"usernames" : data });
        $("#" + viewId).html(htmlOutput)
        $("#" + countId).html(data.length)
      })
      .fail(function(x){
        console.log(x)
        alert('error')
      })
  }

  renderUsernames("/followers", "followers", "followersCount")
  renderUsernames("/following", "following", "followingCount") 

});