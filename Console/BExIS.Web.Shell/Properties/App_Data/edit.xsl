<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <!--xmlns:myUtils="abc:ServerUtils">-->

  <xsl:param name="postBackUrl"/>
  <xsl:template match="/">
    <!--<xsl:variable name="postUrl">
        <xsl:choose>
          <xsl:when test="string-length($areaName) &gt; 0">
            <xsl:copy-of select="concat(concat($areaName, '/', $controllerName), '/', $actionName)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="concat($controllerName, '/', $actionName)" />
          </xsl:otherwise>
        </xsl:choose>

      </xsl:variable>-->
    <form method="post" action="{$postBackUrl}">
      <h2>Information (edit):</h2>
      <table border="0">
        <xsl:for-each select="entity/field">
          <tr>
            <td>
              <xsl:value-of select="@displayName"/>
              <!--<xsl:value-of select="ServerUtils:FormatName(FirstName, LastName)" />-->
            </td>
            <td>
              <input type="text">
                <xsl:attribute name="id">
                  <xsl:value-of select="@id" />
                </xsl:attribute>
                <xsl:attribute name="name">
                  <xsl:value-of select="@id" />
                </xsl:attribute>
                <xsl:attribute name="value">
                  <xsl:value-of select="value" />
                </xsl:attribute>
              </input>
            </td>
          </tr>
        </xsl:for-each>
      </table>
      <br />
      <input type="submit" id="btn_sub" name="btn_sub" value="Submit" />
      <input type="reset" id="btn_res" name="btn_res" value="Reset" />
    </form>
  </xsl:template>

</xsl:stylesheet>