<table width="100%" cellpadding="0" cellspacing="0">
    <tr>
<? if ($userinfo['loggedin']) { ?>       	
      <td class="menuItems">
        <a href="index.php">home</a> | <a href="account.php" accesskey="a">account</a> | <a href="reminders.php">reminders</a> | <a href="allowlist.php">allow list</a> | <a href="http://www.remindme.cc/wiki">help</a> | <a href="contact.php">contacts</a> 
      </td>
      <td class="menuItems">
				<a href="login.php?logout=true">logout</a>&nbsp;
      </td>
<? } else { ?>
			<td class="menuItems">
				<a href="index.php">home</a> | <a href="registerform.php">register</a> | <a href="http://www.remindme.cc/wiki/index.php?title=User%27s_Guide">how it works</a> | <a href="http://www.remindme.cc/wiki">help</a> | <a href="contact.php">contact</a>
			</td>
			<td class="menuItems">
				<!--<a href="#" id="login_form_parent">login</a>&nbsp;-->
				<a href="login.php">login</a>&nbsp;
			</td>
<? } ?>        
    </tr>
</table>