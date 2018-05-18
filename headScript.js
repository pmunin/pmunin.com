//script loaded by pmunin.com at loader

// https://pmunin.github.io/pmunin.com/util.js
// <script src="https://gist-it.appspot.com/https://github.com/pmunin/homepage/blob/master/views/layout.ejs?footer=minimal"></script>

function githubFile(fileUrl)
{
    console.log("rendering code of github file:"+fileUrl);
    document.write("<script src='http://gist-it.appspot.com/http://github.com/"+fileUrl+"'></script>")
}