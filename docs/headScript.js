//script should be injected in the head of blogger's template HTML

/**
 * Renders content of the github source file or gist to current document
 * 
 * @param {string} fileUrl 
 */
function githubFile(fileUrl)
{
    //see documentation here: http://gist-it.appspot.com/
    // example: <script src="https://gist-it.appspot.com/https://github.com/pmunin/homepage/blob/master/views/layout.ejs?footer=minimal"></script>

    if(fileUrl.indexOf("gist.github.com")>-1)
        document.write("<script src='"+fileUrl+"'></script>");
    else
        document.write("<script src='https://gist-it.appspot.com/"+fileUrl+"'></script>")
}


function getJsonFromUrl(hashBased) {
    //from here: https://stackoverflow.com/a/8486188/508797
    var query;
    if(hashBased) {
      var pos = location.href.indexOf("?");
      if(pos==-1) return [];
      query = location.href.substr(pos+1);
    } else {
      query = location.search.substr(1);
    }
    var result = {};
    query.split("&").forEach(function(part) {
      if(!part) return;
      part = part.split("+").join(" "); // replace every + with space, regexp-free version
      var eq = part.indexOf("=");
      var key = eq>-1 ? part.substr(0,eq) : part;
      var val = eq>-1 ? decodeURIComponent(part.substr(eq+1)) : "";
      var from = key.indexOf("[");
      if(from==-1) result[decodeURIComponent(key)] = val;
      else {
        var to = key.indexOf("]",from);
        var index = decodeURIComponent(key.substring(from+1,to));
        key = decodeURIComponent(key.substring(0,from));
        if(!result[key]) result[key] = [];
        if(!index) result[key].push(val);
        else result[key][index] = val;
      }
    });
    return result;
  }