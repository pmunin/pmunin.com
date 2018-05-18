//script loaded by pmunin.com at loader

function githubFile(fileUrl)
{
    //see documentation here: http://gist-it.appspot.com/
    // example: <script src="https://gist-it.appspot.com/https://github.com/pmunin/homepage/blob/master/views/layout.ejs?footer=minimal"></script>

    console.log("rendering code of github file:"+fileUrl);
    document.write("<script src='https://gist-it.appspot.com/http://github.com/"+fileUrl+"'></script>")
}