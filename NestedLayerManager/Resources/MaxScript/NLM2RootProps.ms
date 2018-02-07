
(
	nestedLayerManagerRootNode = attributes "nestedLayerManager" (
		parameters main (
			folderDirs type:#StringTab tabSizeVariable:true	animatable:false
			foldercolours type:#ColorTab tabSizeVariable:true animatable:false
			layerDirs type:#StringTab tabSizeVariable:true	animatable:false
			layerRefs type:#MaxObjectTab tabSizeVariable:true animatable:false
			dataVersion type:#Float animatable:false
		)
		function cleanLayerCA = (
			try (
				layerRefs = rootNode.nestedLayerManager.layerRefs
				layerDirs = rootNode.nestedLayerManager.layerDirs
				layers = for a=0 to layerManager.count-1 collect (layerManager.getLayer a).layerAsRefTarg
				for a=layerRefs.count to 1 by -1 do (
					if findItem layers layerRefs[a] == 0 do (
						deleteItem layerRefs a
						deleteItem layerDirs a
					)
				)
				rootNode.nestedLayerManager.layerRefs = layerRefs
				rootNode.nestedLayerManager.layerDirs = layerDirs
				if isProperty nestedLayerManager "showLog" do (
					if nestedLayerManager.showLog == true do (
						format "[NLM] Undefined Layer Refs Removed\n"
					)
				)
				true
			) catch(false)
		)
	)
custAttributes.add rootNode nestedLayerManagerRootNode
)