# Notes for space-track.org API

It uses the Alpha-5 format.

## Formats

### TLE Format / 2LE

Two-line Element (TLE)
Describe ONE Elset (Element Set) of positional data

### 3LE Format

Three-line Element (TLE)

### Alpha-5 format

The Alpha-5 i an object numbering format. Uses TLE format to replace the 1ST digit by LETTER, to represent 240 000 more object.

So become 100000 => A0000 / 339999 => Z9999 / 301928 => W1928

### OMM

XML
Orbit Mean-Elements Message (OMM)

### Type of object

payload, rocket body (R/B) or piece of debris (DEB)

If no type, object is not yet in the SATCAT

## Type of catalogs ?

### SATCAT

<https://www.space-track.org/#catalog>
Listing of all objects with their associated elsets

### GP / Gp_History

<https://www.space-track.org/documentation#Request%20Classes>

<https://www.facebook.com/356438544394098/posts/our-new-general-perturbations-gp-class-is-an-efficient-listing-of-the-newest-sgp/3010478225656770/>

### DECAY

 TODO

## NASA Spacecraft Conjunction Assessment and Collision Avoidance Best Practices Handbook

<https://nodis3.gsfc.nasa.gov/OCE_docs/OCE_51.pdf>

TLE is not recomended because not precise enough

ALl object are usually bigger than 10cm
requirement de taille pour requirement de trackabilité
requirement de contact des gens en cas de prévision de conjonction

USSPACECOM currently provides a free conjunction assessment service that performs
conjunction assessment screenings on behalf of an O/O and sends proximity warnings (CDMs)

## Querys

|Query type | URL |
|----|----|

## Querying limits

Type Frequency Details
CDM  3 / day Once every 8 hours for all constellation Conjunction Data Messages (CDM)
CDM  1 / hour Once every hour for a specific conjunction event (Note: The 18 SDS will continue to send Close Approach (CA) emails to satellite owners/operators)
TLE  1 / hour Once every hour for TLEs. Please randomly choose a minute for this query that is not at the top or bottom of the hour.
SATCAT  1 / day Once per day after 1700 (UTC) for SATCAT data
BOXSCORE  1 / day Once per day after 1700 (UTC) for Box Score data
60-DAY DECAY  1 / week Once per week, on Wednesdays after 1700 (UTC), for 60-Day Decay data

## Resources

TLE data format : <https://www.space-track.org/documentation#/tle>
Complimented by <https://www.space-track.org/documentation#/legend>
Graph  and averages : <https://www.space-track.org/#spaceOpsTempo>
<https://orbit.ing-now.com/geosynchronous-orbit/>

Fetch all update for one day | <https://www.space-track.org/basicspacedata/query/class/gp_history/CREATION_DATE/2023-03-25--2023-03-26/orderby/NORAD_CAT_ID,EPOCH/format/tle/emptyresult/show>
all update in long period|<https://www.space-track.org/basicspacedata/query/class/gp/CREATION_DATE/2000-01-01--2023-03-26/orderby/NORAD_CAT_ID,EPOCH/format/tle/emptyresult/show>|

<https://www.space-track.org/documentation#api-sampleQueries>

object in orbit : <https://www.space-track.org/#ssr>

✅ All GP elsets : <https://www.space-track.org/basicspacedata/query/class/gp/orderby/CCSDS_OMM_VERS> asc/emptyresult/show

## Glossary

<https://www.space-track.org/documentation#/legend>

- SATCAT = public SATellite CATalog

- ELSETs =  Element Set (of positional data)

- Analyst Objects
  - They are objects that are not in the SATCAT, but are tracked by the US Space Surveillance Network (SSN). They are not here because not enough data about them.
    However, some analyst objects are in the SATCAT, listed between 80000 and 89999. They are here because cna't be traced to any launch but can still be tracked.

- ephemeris / plur. = Ephemerdies
  - a table or data file giving the calculated positions of a celestial object at regular intervals throughout a period.

## Piste mapping

- Refaire une vue 2D du modèle 3D, avec les couelurs pour les pays

- Travailler sur les constellation de satelites ( satelite appartannt au même groupe / ayant le même nom)
