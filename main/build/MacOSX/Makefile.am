MAC_APP_DIR=MonoDevelop.app
APP_NAME=MonoDevelop
MACOS=$(MAC_APP_DIR)/Contents/MacOS
MAC_APP_LIB_DIR=$(MACOS)/lib/monodevelop
ADDINS=$(MACOS)/lib/monodevelop/AddIns
EXTRAS=$(top_srcdir)/../extras
ARTIFACTS=$(top_srcdir)/../../artifacts
MD_CONFIGURE=$(top_srcdir)/../scripts/configure.sh

PACKAGE_UPDATE_ID=$(shell $(MD_CONFIGURE) get-releaseid)
BUNDLE_VERSION=$(shell $(MD_CONFIGURE) get-version)

CLEANFILES = render.exe
#DISTCLEANFILES =
EXTRA_DIST = dmg-bg.png DS_Store Info.plist.in make-dmg-bundle.sh render.exe

all: app

render.exe: render.cs
	gmcs -r:System.Drawing render.cs

dmg: render.exe app
	./make-dmg-bundle.sh

monostub: monostub.m $(MONOSTUB_EXTRA_SOURCES)
	gcc -Wall -mmacosx-version-min=10.6 -m32 -o $@ $^ -framework AppKit

clean-local:
	rm -rf MonoDevelop.app
	rm -f MonoDevelop*.dmg

app: monostub
	@echo ""
	@echo "Creating directories in app bundle"
	@echo ""
	mkdir -p $(MAC_APP_DIR)/Contents/{MacOS,Resources}
	mkdir -p $(MAC_APP_DIR)/Contents/MacOS/{lib,share}
	mkdir -p $(MAC_APP_LIB_DIR)

#	@echo ""
#	@echo "Copying install prefix into app bundle"
#	@echo ""
#	@if [[ "$(prefix)" == /Library* ]] || [[ "$(prefix)" == /usr* ]]; then \
#		echo "WARNING: cannot merge system prefix $(prefix)"; \
#	else \
#		echo "cp -pR $(prefix)/* $(MAC_APP_DIR)/Contents/MacOS/"; \
#		cp -pR $(prefix)/* $(MAC_APP_DIR)/Contents/MacOS/; \
#	fi

	@echo ""
	@echo "Copying MonoDevelop into app bundle"
	@echo ""
	cp -pR ../AddIns $(MAC_APP_LIB_DIR)
	cp -pR ../data $(MAC_APP_LIB_DIR)
	cp -pR ../bin $(MAC_APP_LIB_DIR)
	cp -pR ../locale $(MAC_APP_DIR)/Contents/MacOS/share
	mkdir -p $(MAC_APP_DIR)/Contents/MacOS/share/monodevelop
	cp ../../COPYING $(MAC_APP_DIR)/Contents/MacOS/share/monodevelop/COPYING.LGPL2.1

	sed -e "s/@BUNDLE_VERSION@/$(BUNDLE_VERSION)/" -e "s/@ASSEMBLY_VERSION@/$(ASSEMBLY_VERSION)/" -e "s/@APP_NAME@/$(APP_NAME)/" -e "s|@RELEASE_ID@|$(PACKAGE_UPDATE_ID)|" Info.plist.in > $(MAC_APP_DIR)/Contents/Info.plist
	cp ../../theme-icons/Mac/*.icns $(MAC_APP_DIR)/Contents/Resources/

# copy monodevelop.pc and monodevelop-core-addins.pc to Contents/MacOS/lib/pkgconfig
	mkdir -p $(MACOS)/lib/pkgconfig
	cp ../../*.pc $(MACOS)/lib/pkgconfig

# Copy main/monodevelop.xml to MonoDevelop.app/Contents/MacOS/share/mime/packages
	mkdir -p $(MACOS)/share/{applications,mime,icons,locale,man}
	mkdir -p $(MACOS)/share/mime/packages
	cp ../../monodevelop.xml $(MACOS)/share/mime/packages
	cp ../../monodevelop.desktop $(MACOS)/share/applications

# Native launch scripts
	@echo ""
	@echo "Copying launch scripts into app bundle"
	@echo ""
	cp monostub $(MACOS)/MonoDevelop
	cp monostub $(MACOS)/mdtool

# Remove addins that don't work on OSX
	@echo ""
	@echo "Removing unsupported addins"
	@echo ""

	rm -rf  $(ADDINS)/GnomePlatform* \
	  $(ADDINS)/WindowsPlatform*

# remove stuff we don't need
	@echo ""
	@echo "Removing unneeded files"
	@echo ""

	rm -rf $(MACOS)/lib/monodevelop/bin/UnitTests.dll* \
	  $(MACOS)/lib/monodevelop/bin/TestResult.xml \
	  $(MACOS)/lib/monodevelop/bin/addin-db* \
	  $(MACOS)/lib/monodevelop/bin/local \
	  $(MACOS)/lib/monodevelop/bin/repository-cache* \
	  $(MACOS)/lib/monodevelop/bin/addins-setup.config \
	  $(MACOS)/lib/monodevelop/AddIns/MonoDevelop.TextTemplating/Mono.TextTemplating.Tests*

# add extras known to work on Mac, if they exist
	@echo ""
	@echo "Copying supported extras into app bundle"
	@echo ""

	@for ADDIN in MonoDevelop.Database; do \
		(ls $(EXTRAS)/$${ADDIN}/build/* > /dev/null 2>&1 && \
			echo "Copying $${ADDIN}" && \
			mkdir -p $(MAC_APP_LIB_DIR)/AddIns/$${ADDIN} && \
			cp -pR $(EXTRAS)/$${ADDIN}/build/* $(MAC_APP_LIB_DIR)/AddIns/$${ADDIN}/ ) \
		|| (echo "Skipping $${ADDIN}" && echo "$${ADDIN}" >> missing-stuff)  ; \
	done

# add the binary artifacts if they exist
	@echo ""
	@echo "Copying binary artifacts into app bundle"
	@echo ""

	mkdir -p $(MACOS)/lib/pkgconfig
	mkdir -p $(MACOS)/MonoDoc.app/
	test ! -d $(ARTIFACTS)/MonoDoc.app ||  cp -pR $(ARTIFACTS)/MonoDoc.app/* $(MACOS)/MonoDoc.app/

	cp -r MDMonitor.app $(MACOS)

# update revision in updateinfo
	@echo ""
	@echo "Updating build information"
	@echo ""

	$(MD_CONFIGURE) gen-buildinfo "$(MACOS)/lib/monodevelop/bin"

# touch the directory so that update dependencies work
	touch MonoDevelop.app

	@echo ""
	@cat "$(MACOS)/lib/monodevelop/bin/buildinfo"
	@if test -f missing-stuff; then echo ""; echo "Missing stuff:"; cat missing-stuff; fi
	@echo ""
	@rm -f missing-stuff

.PHONY: app dmg
