<?xml version="1.0" encoding="iso-8859-1" ?>
<!--

Biodiversity Exploratories Information System

The Biodiversity Exploratories Information System
from the Central Data Management in the Biodiversity Exploratories project
is published under
Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Germany (CC BY-NC-SA 3.0)

http://creativecommons.org/licenses/by-nc-sa/3.0/de/deed.en

Developed by the Central Data Management of Biodiversity Exploratories
http://www.biodiversity-exploratories.de/

-->
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:bgc="http://www.bgc-jena.mpg.de"
xmlns:xsd="http://www.w3.org/2001/XMLSchema"
xmlns:asp="xxx"
xmlns:cc1="xxx"
xmlns:exsl="urn:schemas-microsoft-com:xslt"
extension-element-prefixes="exsl"
>
  <xsl:output method="html" version="4.0" indent="yes"/>
  <xsl:template match="Metadata">
    <html>
      <head>
        <title>Publication details</title>
      </head>
      <body>
        <div class="box">
          <h3>
            Metadata 
          </h3>
          <table>
            <xsl:apply-templates/>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>
  
  
  <xsl:template match="*">

    
    <tr>
        <td class="headerText">
          <xsl:value-of select="name()" />
        </td>
        <td>
          <hr></hr>
        </td>
    </tr>
    <xsl:for-each select=".//*">
      <tr>
        <td class="key">
        <xsl:value-of select="name()" />
         </td>
     
        <td class="alternate1">
          <xsl:value-of select="." />
        </td>
      </tr>
      
    </xsl:for-each>
  </xsl:template>   
    
 
</xsl:stylesheet>