import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 2,
    duration: '30s',
    thresholds: {
        http_req_duration: ['p(95)<200'], // Дуже швидка перевірка
    },
};

export default function () {
    const res = http.get('http://localhost:5263/api/auctions');
    check(res, { 'status is 200': (r) => r.status === 200 });
    sleep(1);
}