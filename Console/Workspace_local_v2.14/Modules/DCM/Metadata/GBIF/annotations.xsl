<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema"  xmlns:redirect="org.apache.xalan.xslt.extensions.Redirect" extension-element-prefixes="redirect">
	<xsl:output method="xml"/>
	<xsl:template match="/">
		
	<xsl:param name="element" select="/xs:schema/xs:element"/>

  
			
	 <redirect:write select="concat('annotations', '.html')">
		
				<html>
					<head>
						<style>
						body {font-family: Verdana, Helvetica, sans-serif; font-size:100%; color: black; background-color:white ;}  
						dt {font-size: 110%; font-weight: bold; color: teal ; background-color:snow; margin-top:25px;margin-bottom:1px; padding-top:5px; height:5px;} 
						dd {font-size: 100%;  margin-top:5px;margin-bottom:10px; padding-top:0px; height:1px;} 
						</style>
						<title/>
					</head>
					<body>
					<div>
					<p>The definitions of the elements are taken from the following sources:</p>
					<p>ISO 19139: <a href="http://www.fgdc.gov/standards/projects/incits-l1-standards-projects/NAP-Metadata/napMetadataProfileV101.pdf">ISO 19139: North American Profile of ISO19115:2003 - Geographic information - Metadata</a></p>
					<p>EML: <a href="http://knb.ecoinformatics.org/software/eml/eml-2.1.1/index.html">Ecological Metadata Language (EML) Specification</a></p>
					<p>NCD: <a href="http://www.tdwg.org/standards/312/">Natural Collections Descriptions (NCD): A data standard for exchanging data describing natural history collections</a></p>
					</div>
					<xsl:for-each select="$element">
					<xsl:sort select="@name"/>
					<a>
						<xsl:attribute name="name">
						<xsl:value-of select="@name"/>
						</xsl:attribute>
					</a>
					<dt><xsl:value-of select="@name"/></dt>
					
					<dd><xsl:value-of select="xs:annotation/xs:documentation"/></dd>
			
					</xsl:for-each>
			
							
			
					</body>
				</html>
		</redirect:write>
</xsl:template>
</xsl:stylesheet>
