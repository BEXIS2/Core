<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <xsl:if test="price &gt; 10">
      
    </xsl:if>
    <xsl:choose>
      <xsl:when test="count(catalog/cd) &gt; 1">
        <!--<xsl:for-each select="catalog/cd[1]">-->
          <!--<xsl:apply-templates select="."/>-->
        <!--</xsl:for-each>-->
        <xsl:apply-templates select="catalog/cd[2]"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates />
      </xsl:otherwise>
    </xsl:choose>    
  </xsl:template>

  <xsl:template match="cd">
    <p>
      <xsl:apply-templates select="title"/>
      <xsl:apply-templates select="artist"/>
    </p>
  </xsl:template>

  <xsl:template match="title">
    Title: <span style="color:#ff0000">
      <xsl:value-of select="."/>
    </span>
    <br />
  </xsl:template>

  <xsl:template match="artist">
    Artist: <span style="color:#00ff00">
      <xsl:value-of select="."/>
    </span>
    <br />
  </xsl:template>

</xsl:stylesheet>