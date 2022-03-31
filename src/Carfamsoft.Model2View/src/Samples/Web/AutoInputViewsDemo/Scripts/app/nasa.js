
(function (glob) {
    const API_KEY = 'kWOQjk1ig7MuilTaTuQB5z5coEsBLJblOC7Qh0v6';
    const APOD_URL = `https://api.nasa.gov/planetary/apod?api_key=${API_KEY}&date=`;
    const NEOFEED_URL = `https://api.nasa.gov/neo/rest/v1/feed?start_date=START_DATE&end_date=END_DATE&api_key=${API_KEY}`;
    const NEO_BROWSE_URL = `https://api.nasa.gov/neo/rest/v1/neo/browse?api_key=${API_KEY}`;
    let storedAPOD;

    const NASA = {
        fetchApod: function (date, onsuccess, onfail) {
            date = toIsoDateString(date);

            const LOCAL_KEY = 'NASA.APOD';
            const localItem = localStorage.getItem(LOCAL_KEY) || '{ "items": [] }';
            const stored = storedAPOD || (storedAPOD = JSON.parse(localItem));
            const { items = [] } = stored;
            const item = items.find(p => p.date === date);

            if (!!item) {
                onsuccess && onsuccess(item.data);
            } else {
                console.info('Fetching data from the server...');
                axios.get(APOD_URL + date).then(res => {
                    console.log('Done!');
                    items.push({
                        date,
                        data: res.data
                    });
                    stored.items = items;
                    localStorage.setItem(LOCAL_KEY, JSON.stringify(stored));
                    onsuccess && onsuccess(res.data);
                }).catch(err => {
                    console.error(err);
                    onfail && onfail(err);
                });
            }
        },
        fetchNeoFeed: function (startDate, endDate, onsuccess, onfail) {
            startDate = toIsoDateString(startDate);
            endDate && (endDate = toIsoDateString(endDate));

            const url = NEOFEED_URL
                .replace('START_DATE', startDate)
                .replace('END_DATE', endDate);

            axios.get(url).then(res => {
                onsuccess && onsuccess(res.data);
            }).catch(err => {
                console.error(err);
                onfail && onfail(err);
            });
        },
        createApodApp: function (el, date) {
            return new Vue({
                el,
                data: {
                    apod: {},
                    error: {},
                    fetching: false
                },
                computed: {
                    image: function () {
                        return this.isImage();
                    },
                    video: function () {
                        return this.isVideo();
                    },
                    embeddedVideo: function () {
                        return !this.isYoutube() && this.isEmbedded();
                    },
                    youtubeVideo: function () {
                        return this.isYoutube();
                    },
                    otherVideo: function () {
                        return this.isVideo() && !(this.isYoutube() || this.isEmbedded());
                    }
                },
                methods: {
                    dayPrev: function (date, onsuccess) {
                        date = new Date(date).addDays(-1);
                        fetchApodData(date, this, onsuccess);
                    },
                    dayNext: function (date, onsuccess) {
                        date = new Date(date).addDays(1);
                        fetchApodData(date, this, onsuccess);
                    },
                    pushHistory: function (options) {
                        const date = toIsoDateString(options.date);
                        const url = replaceQueryParam('date', date);
                        history.pushState(options.data, document.title, url);
                    },
                    isImage: function () {
                        return this.apod.media_type === 'image';
                    },
                    isVideo: function () {
                        return this.apod.media_type === 'video';
                    },
                    isYoutube: function () {
                        return this.isVideo() &&
                            (this.apod.url.indexOf('youtube') > -1 ||
                            this.apod.url.indexOf('youtu.be') > -1);
                    },
                    isEmbedded: function () {
                        return this.isVideo() && this.apod.url.indexOf('embed') > -1;
                    }
                },
                created: function () {
                    fetchApodData(date, this);
                    glob.onpopstate = event => {
                        if (event.state != null) this.apod = event.state;
                    };
                }
            });

            function fetchApodData(date, receiver, onsuccess) {
                receiver.fetching = true;
                NASA.fetchApod(date, data => {
                    receiver.fetching = false;
                    receiver.apod = data;
                    onsuccess && onsuccess({ date, data });
                }, error => {
                    receiver.fetching = false;
                    receiver.error = error;
                });
            }
        },
        createNeoFeedApp: function (el, startDate, endDate) {
            return new Vue({
                el,
                data: {
                    feed: {},
                    showSummary: true,
                    error: {}
                },
                created: function () {
                    NASA.fetchNeoFeed(startDate, endDate,
                        data => this.feed = data,
                        error => this.error = error);
                },
                methods: {
                    getCloseApproachDate: function (obj) {
                        if (obj.close_approach_data.length > 0)
                            return obj.close_approach_data[0].close_approach_date_full;
                        return 'N/A';
                    },
                    getDetailUrl: function (baseUrl, id) {
                        return `${baseUrl}/${id}`;
                    },
                    remove: function (date, obj_key) {
                        Vue.delete(this.feed.near_earth_objects[date], obj_key);
                    },
                    removeObject: function (date) {
                        Vue.delete(this.feed.near_earth_objects, date);
                    },
                    objectCount: function (date) {
                        return this.feed.near_earth_objects[date].length;
                    },
                    toDateString: function (date) {
                        return new Date(date).toDateString();
                    },
                    shortestMissDistance: function (date, unit) {
                        unit || (unit = 'kilometers');
                        const items = this.feed.near_earth_objects[date]
                            .filter(obj => obj.close_approach_data.length > 0)
                            .map(obj => ({
                                name: obj.name,
                                distance: obj.close_approach_data[0].miss_distance[unit]
                            }))
                            .sort((a, b) => a.distance - b.distance);

                        if (items.length > 0)
                            return `${items[0].name} has the shortest miss distance: ${items[0].distance} ${unit}` ;
                        return '';
                    },
                    getRowStyle: function (date) {
                        const obj = this.feed.near_earth_objects[date];
                        if (obj.length > 0 && obj[0].close_approach_data.length === 0)
                            return { border: 'solid 3px red;', color: 'red' };
                    }
                }
            });
        }
    }

    glob.NASA = NASA;
})(window);