# SpeckleUnity

[![registry](https://img.shields.io/badge/registry-v0.6.4--preview-orange)](https://img.shields.io/badge/registry-v0.6.4--preview-orange) [![openupm](https://img.shields.io/npm/v/com.open.speckleunity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.open.speckleunity/) ![![codeCoverage](https://img.shields.io/badge/coverage-53.3%25-orange)](https://img.shields.io/badge/coverage-53.3%25-orange) [![speckleCoreVersion](https://img.shields.io/badge/SpeckleCore-v1.8.0(modified)-brightgreen)](https://img.shields.io/badge/SpeckleCore-v1.8.0(modified)-brightgreen) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen)](http://makeapullrequest.com)

In this repository you will find the source code, assets and project settings of the SpeckleUnity package for Unity app development (Unity 2019.3 or newer recommended) along with a DocFX project containing documentation assets and articles.

## NOTICE

* [Documentation can be found here](https://speckleworks.github.io/SpeckleUnity/).
* [Supported platforms can be found here](https://speckleworks.github.io/SpeckleUnity/articles/supportedPlatforms.html).
* Only displays mesh, point, and line data types. Breps, Curves and PolyLines are converted using their display values.
* Does not use the Speckle Kit workflow for portability reasons. Any additional Speckle Kit DLLs must be added manually to the Unity Project. 
* Code coverage measured using the Unity Code Coverage package.

## How To Install

See instructions in [this](https://speckleworks.github.io/SpeckleUnity/articles/howToInstall.html) part of the documentation.

---

## Roadmap

> Roadmap is subject to change. Last reviewed 22nd of April 2020.

| Version | Defining Feature                  						  				         |
| ------- | -------------------------------------------------------------------------------- |
| ~0.1~   | ~First prototype release~   								 			         |
| ~0.2~   | ~Restructure and release as UPM package~								         |
| ~0.3~   | ~New component workflow and custom materials assigned via inspector~			 |
| ~0.4~   | ~Spawn geometry in transform heirarchy based on layer data~		    	         |
| ~0.5~   | ~User login API, get Stream API and no dependency on a local install of Speckle~ |
| ~0.6~   | ~Android Support, Rendering Rule API and Async Refactor~                         |
| 0.7     | Local caching of Speckle streams			                                     |
| 0.8     | Implement SpeckleUnitySender API    			                                 |
