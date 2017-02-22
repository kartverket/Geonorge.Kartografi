<?xml version="1.0" encoding="UTF-8"?>
<StyledLayerDescriptor xmlns="http://www.opengis.net/sld" xmlns:ogc="http://www.opengis.net/ogc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" version="1.1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xsi:schemaLocation="http://www.opengis.net/sld http://schemas.opengis.net/sld/1.1.0/StyledLayerDescriptor.xsd" xmlns:se="http://www.opengis.net/se">
  <NamedLayer>
    <se:Name>Punkt_svg</se:Name>
    <UserStyle>
      <se:Name>Punkt_svg</se:Name>
      <se:FeatureTypeStyle>
        <se:Rule>
          <se:Name>Svg-test</se:Name>
           <PointSymbolizer>
		   <Graphic>
			 <ExternalGraphic>
			   <OnlineResource xlink:type="simple" xlink:href="http://blackicemedia.com/presentations/2013-02-hires/img/awesome_tiger.svg" />
			   <Format>image/svg+xml</Format>
			 </ExternalGraphic>
			 <Size>4</Size>
		   </Graphic>
		 </PointSymbolizer>
        </se:Rule>
      </se:FeatureTypeStyle>
    </UserStyle>
  </NamedLayer>
</StyledLayerDescriptor>
