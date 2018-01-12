-- MySQL dump 10.11
--
-- Host: localhost    Database: remindme_reminder
-- ------------------------------------------------------
-- Server version	5.0.67-community

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `ad_history`
--

DROP TABLE IF EXISTS `ad_history`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `ad_history` (
  `id` int(11) NOT NULL default '0',
  `datetime` datetime NOT NULL default '0000-00-00 00:00:00',
  `touser` varchar(100) NOT NULL default '',
  `toconn` varchar(100) NOT NULL default ''
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `ad_info`
--

DROP TABLE IF EXISTS `ad_info`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `ad_info` (
  `id` int(11) NOT NULL auto_increment,
  `ownerid` varchar(100) NOT NULL default '',
  `maxcount` int(11) NOT NULL default '0',
  `currentcount` varchar(100) NOT NULL default '',
  `delivered` tinyint(1) NOT NULL default '0',
  `type` tinytext,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=13 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `ad_text`
--

DROP TABLE IF EXISTS `ad_text`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `ad_text` (
  `id` int(11) NOT NULL default '0',
  `text` text NOT NULL,
  `html` text NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_allow_lists`
--

DROP TABLE IF EXISTS `system_allow_lists`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_allow_lists` (
  `owner` varchar(100) NOT NULL default '',
  `buddy` varchar(100) NOT NULL default '',
  `num_allowed` int(11) NOT NULL default '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_bots`
--

DROP TABLE IF EXISTS `system_bots`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_bots` (
  `name` varchar(100) NOT NULL default '',
  `users` int(11) NOT NULL default '0',
  `aim_name` varchar(100) NOT NULL default '',
  `msn_name` varchar(100) NOT NULL default '',
  `icq_name` varchar(100) NOT NULL default '',
  `yahoo_name` varchar(100) NOT NULL default '',
  `email_name` varchar(100) NOT NULL default '',
  `active` tinyint(4) default NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_contacts`
--

DROP TABLE IF EXISTS `system_contacts`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_contacts` (
  `id` int(11) NOT NULL auto_increment,
  `userid` varchar(100) NOT NULL default '0',
  `service` varchar(100) NOT NULL default '',
  `login` varchar(100) NOT NULL default '',
  `priority` int(11) NOT NULL default '0',
  `verified` tinyint(1) NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=2186 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_reminder_count`
--

DROP TABLE IF EXISTS `system_reminder_count`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_reminder_count` (
  `count` int(11) NOT NULL default '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_reminders`
--

DROP TABLE IF EXISTS `system_reminders`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_reminders` (
  `bot` varchar(100) NOT NULL default '',
  `user` varchar(100) NOT NULL default '',
  `datetime` datetime NOT NULL default '0000-00-00 00:00:00',
  `msg` text NOT NULL,
  `delivered` tinyint(4) NOT NULL default '0',
  `id` int(1) NOT NULL auto_increment,
  `servertime` datetime NOT NULL default '0000-00-00 00:00:00',
  `deliveredtime` datetime default '0000-00-00 00:00:00',
  `creator` varchar(20) default NULL,
  `deliveredname` varchar(100) default NULL,
  `deliveredconn` varchar(100) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=5411 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_repeaters`
--

DROP TABLE IF EXISTS `system_repeaters`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_repeaters` (
  `repid` int(11) NOT NULL auto_increment,
  `remid` int(11) NOT NULL default '0',
  `pattern` varchar(100) NOT NULL default '',
  `count` int(11) NOT NULL default '0',
  `disabled` tinyint(1) NOT NULL default '0',
  `expiration` datetime NOT NULL default '0000-00-00 00:00:00',
  PRIMARY KEY  (`repid`)
) ENGINE=MyISAM AUTO_INCREMENT=310 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_subscribe_plans`
--

DROP TABLE IF EXISTS `system_subscribe_plans`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_subscribe_plans` (
  `id` int(11) NOT NULL default '0',
  `plan_name` varchar(100) NOT NULL default '',
  `cost` double NOT NULL default '0',
  `num_reminders` int(11) NOT NULL default '0',
  `allow_repeat_reminders` tinyint(1) NOT NULL default '0',
  `num_ptp_reminders` int(11) NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_suggestions`
--

DROP TABLE IF EXISTS `system_suggestions`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_suggestions` (
  `sid` int(11) NOT NULL auto_increment,
  `uid` varchar(100) NOT NULL default '',
  `subject` int(11) NOT NULL default '0',
  `text` text NOT NULL,
  `datetime` datetime NOT NULL default '0000-00-00 00:00:00',
  `poll` tinyint(1) NOT NULL default '0',
  `response` text NOT NULL,
  `votes` int(11) NOT NULL default '0',
  PRIMARY KEY  (`sid`)
) ENGINE=MyISAM AUTO_INCREMENT=31 DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `system_users`
--

DROP TABLE IF EXISTS `system_users`;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
CREATE TABLE `system_users` (
  `id` varchar(100) NOT NULL default '',
  `class` varchar(100) NOT NULL default 'user',
  `email` varchar(100) NOT NULL default '',
  `contact_order` varchar(100) NOT NULL default '',
  `aim_name` varchar(16) NOT NULL default '',
  `msn_name` varchar(100) NOT NULL default '',
  `icq_name` varchar(100) NOT NULL default '',
  `bot` varchar(100) NOT NULL default '',
  `timezone` varchar(10) NOT NULL default '-0500',
  `password` varchar(100) NOT NULL default '',
  `firstname` varchar(100) NOT NULL default '',
  `surname` varchar(100) NOT NULL default '',
  `plan` smallint(4) NOT NULL default '0',
  `DLS` tinyint(1) NOT NULL default '1',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
SET character_set_client = @saved_cs_client;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2008-12-08  1:17:03
