<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                exclude-result-prefixes="msxsl"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:fn="http://www.functx.com"
                xmlns:func="http://www.functx.com"
>

  <xsl:output method="html" indent="yes"/>
  <xsl:strip-space elements="*"/>

  <xsl:template match="/">
    <div id="treeView">
      <ul>
        <xsl:apply-templates/>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="xs:element">
    <li xPath="{func:createXPath(.)}">

      <xsl:value-of select="@name"/>
      <xsl:if test="xs:*">
        <ul>
          <xsl:apply-templates/>
        </ul>
      </xsl:if>
    </li>
  </xsl:template>

  <xsl:function name="func:createXPath" as="xs:string" >
    <xsl:param name="pNode" as="node()"/>
    <xsl:value-of select="$pNode/ancestor-or-self::*/name()" separator="/"/>
  </xsl:function>

</xsl:stylesheet>

