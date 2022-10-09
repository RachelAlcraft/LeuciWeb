# Leucippus todo list

## to do
### Dev
Technical
32 bit problem, 
- Whether or not the plan is upgraded I still need to manage memory for cryo-em structures so need to solve it
- Take a subsection of the cube with a buffer for navigation and update it if necessary
- ALso take a slice for the plane navigation
- Consider using only C# and no C++ due to the problem with managed/unmanaged in 32 bit
- the first easy test will be - can it work for cryo-em matrices (locally)? IE the highest res structure 7a6a
-- note the ed is not so easy to find, this one's link is emd_11668.map.gz - same format once unzipped


C++ or C# - 
- maybe I have over complicated it, stick to the simplest thing, use-ability the most important thing
- but it would be useful to have a console app for batch runs too so some way to keep that in mind, a shared library with clear interface at the least

Slice screen
- Enable typing in chimera style atoms, eg A:708@N
- Resolve to coordinates that can be changed
- If the coordinates no longer match the atom grey it out
- No need to browse pdb file, but a link could be useful.
- For nearest neighbour, don’t display Radiant and Laplaciam
- For linear don’t display Laplacian
- Look at what coot does interpolation-wise
- Toggle 3 (or 4) colours
- Toggle contour or heat map
- Toggle width and granularity (restrict on ratio)
- Hover to show nearest atom (over take the current hover box somehow)
- Columns are “Arom” and “(x,y,z)”
- Make it so you can see atoms int he hover data - this link is probably the best: https://plotly.com/python/hover-text-and-formatting/ for the heatmap towards the bottom, 
looks like I might need to do it only for heat map, and make contour the z value only.

User logging
- Some sort of log is needed to record what functionality is most used 
- Perhaps each new connect gets an id which is used as part of the session state and for logging





### Ideas
o	Didn’t discuss much about matrix, but it is a useful to have, and didn’t discuss much about the adjusted pdb files

---
## Done

### Oct 2022



