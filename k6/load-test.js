import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 20 },
        { duration: '1m', target: 50 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // Нормальне навантаження
    },
};

export default function () {
    const res = http.get('http://localhost:5263/api/auctions');
    check(res, { 'status is 200': (r) => r.status === 200 });
    sleep(1);
}