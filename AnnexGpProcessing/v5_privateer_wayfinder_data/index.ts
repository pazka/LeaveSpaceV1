import fs from 'fs';

// read all array files in data dir and append thme together

const files = fs.readdirSync('./data')
const allData = files
    .filter((file) => file.endsWith('.json'))
    .map((file) => {
        const data = fs.readFileSync(`./data/${file}`);
        console.log(`Reading ${file}`);
        const dataJSON = JSON.parse(data.toString());
        console.log(`Parsed ${file} : ${dataJSON.length} objects`);
        return dataJSON;
    })
    .reduce((acc, cur) => {
        return [...acc, ...cur];
    }, []);

try {
    fs.mkdirSync('./dist');
} catch (err) {
}

console.log(`Writing ${allData.length} objects to dist/data.json`);
fs.writeFileSync('./dist/data.json', JSON.stringify(allData, null, 2));

// then, write a count of all properties present or not in all the objects and print this count as csv

console.log('Writing count.csv');
const keyCount : any = {};
const objectTypeCount : any = {};

allData.forEach((obj : any,index : number) => {
    if(index % 1000 === 0){
        console.log(Math.round(index/allData.length * 100),`% Processing ${index} of ${allData.length}`);
    }

    Object.keys(obj).forEach((key) => {
        if (!keyCount[key]) {
            keyCount[key] = 0;
        }
        keyCount[key] += 1;
    });

    const type = obj.status+"/"+obj.objectType
    if (!objectTypeCount[type]) {
        objectTypeCount[type] = 0;
    }
    objectTypeCount[type] += 1;
});

//dump as csv sorted by coun
const csv = Object.keys(keyCount)
.sort((a, b) => keyCount[b] - keyCount[a])
.map((key) => `${key},${keyCount[key]}`)
.join('\n');

fs.writeFileSync('./dist/count.csv', csv);

//dump as csv sorted by count
const csv2 = Object.keys(objectTypeCount)
.sort((a, b) => objectTypeCount[b] - objectTypeCount[a])
.map((key) => `${key},${objectTypeCount[key]}`)
.join('\n');

fs.writeFileSync('./dist/object_type_count.csv', csv2);


