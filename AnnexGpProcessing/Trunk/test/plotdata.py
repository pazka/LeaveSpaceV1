import matplotlib.pyplot as plt
from datetime import datetime
import numpy as numpy

"""
    One line of lte is formated like this : 
1 31895U 99025CKW 23143.92175322  .00002784  00000-0  26659-2 0  9996
2 31895  99.1305 206.4865 0242653 164.2034 196.6856 13.68883600794945

where 23143.92175322 is the epoch of the lte
"""


data = []

with open('19500325--20230525.lte') as file:
    data = file.readlines()
    data = [line.strip() for line in data]
    # get all two line of data
    data = [data[i:i+2] for i in range(0, len(data), 2)]

# extract the UTC epoch from lte
raw_epoch = [line[0][18:32] for line in data]
launch_year = [line[0][10:12] for line in data]

launch_year_without_empty = filter(lambda x: x != '  ', launch_year)

# all values superior to 23 must be suffixed by 19 and the rest by 20
launch_year_without_empty = map(lambda x: '19' + x if int(x) > 23 else '20' + x, launch_year_without_empty)

number_epoch = numpy.array([int(e) for e in launch_year_without_empty])

min_epoch = min(number_epoch)
max_epoch = max(number_epoch)
step = (max_epoch - min_epoch)
    

# count element of the same year and plot the number of elemen for each year
plt.hist(number_epoch, bins=step, range=(min_epoch, max_epoch))
plt.xlabel('Year')
plt.ylabel('#element')
plt.show()