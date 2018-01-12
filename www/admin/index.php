<?php
foreach (glob("*.*") as $filename) {
   echo "<a href='$filename'>$filename</a><br>";
}
?> 