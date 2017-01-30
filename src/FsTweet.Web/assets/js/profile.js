$(function(){

  var tweetReadTmpl = `
    {{#tweets}}
      <div>        
        <div class="tweet_read_view bg-info">
          <span class="text-muted">@{{username.value}} - {{#timeAgo}}{{time}}{{/timeAgo}}</span>
          <p>{{tweet.value}}</p>
        </div>        
      </div>

    {{/tweets}}
  `

  var usernameList = `
    {{#usernames}}
      <div class="well">
        <a href="/{{value}}">@{{value}}</a>
      </div>
    {{/usernames}}
  `

  var timeAgo = function () {
    return function(val, render) {
      return moment(render(val)).fromNow()
    };
  }

  var username = window.location.pathname.substring(1)

  function loadTweets(username) {
    $.getJSON("/tweets/" + username, function(){

    }).done(function(data){
      if (data.length === 0) {
        $("#tweets").html('<p class="bg-warning no_tweets">No tweets found <p></div>')
        return
      }
      var htmlOutput = Mustache.render(tweetReadTmpl, {
        "tweets" : data,
        "timeAgo" : timeAgo
      });
      $("#tweets").html(htmlOutput)
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