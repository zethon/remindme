 <?php
 /*header*/
Header("Content-Type: image/png");

/* initialize a session. */
session_start();

/*We'll set this variable later.*/
$new_string;

/*register the session variable. */
session_register('new_string');

/*You will need these two lines below.*/
echo "<html><head><title>Verification</title></head>";
echo "<body>";

/* set up image, the first number is the width and the second is the height*/
$im = ImageCreate(200, 40);

/*creates two variables to store color*/
$white = ImageColorAllocate($im, 255, 255, 255);
$black = ImageColorAllocate($im, 0, 0, 0);

/*random string generator.*/
/*The seed for the random number*/
srand((double)microtime()*1000000);

/*Runs the string through the md5 function*/
$string = md5(rand(0,9999));

/*creates the new string. */
$new_string = substr($string, 17, 5);

 /*fill image with black*/
ImageFill($im, 0, 0, $black);

 /*writes string */
ImageString($im, 4, 96, 19, $new_string, $white);

/* output to browser*/
ImagePNG($im, "verify.png");
ImageDestroy($im);


/*I plugged our image in like I would any other image.*/
echo "<img src=\"verify.png\">";
echo "<br><br>";
echo "Type the code you see in the image in the box below. (case sensitive)";
echo " <form action=\"formhandler.php\" method=post>";
echo "<input name=\"random\" type=\"text\" value=\"\">";
echo "<input type=\"submit\">";
echo "</form>";
echo "</body>";
echo "</html>";

?>