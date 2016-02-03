<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xsl:output method="text" encoding="utf-8"/>
  <xsl:strip-space elements="*"/>

  <xsl:template match="/*[node()]">
    <xsl:text>[</xsl:text>
    <xsl:apply-templates/>
    <xsl:text>]</xsl:text>
  </xsl:template>


  <xsl:template match="xs:element">

    <xsl:text>{ tagname: "</xsl:text>
    <xsl:value-of select="local-name()"/>
    <xsl:text>", </xsl:text>
    <xsl:apply-templates select="@*"/>
    <xsl:apply-templates select="xs:complexType"/>
    <xsl:text>}</xsl:text>
    <xsl:if test="position() lt last()">, </xsl:if>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:value-of select="name()"/>
    <xsl:text>:"</xsl:text>
    <xsl:value-of select="."/>
    <xsl:text>"</xsl:text>
    <xsl:if test="position() lt last()">, </xsl:if>
  </xsl:template>

  <xsl:template match="xs:complexType">
    <xsl:apply-templates select="xs:sequence"/>
  </xsl:template>

  <xsl:template match="xs:sequence">
    <xsl:text>, sequence:[</xsl:text>
    <xsl:apply-templates select="xs:element"/>
    <xsl:text>]</xsl:text>
  </xsl:template>
</xsl:stylesheet>