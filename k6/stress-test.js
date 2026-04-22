import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 200 }, // Точка відмови
        { duration: '30s', target: 0 },
    ],
};

export default function () {
    const payload = JSON.stringify({ amount: Math.random() * 1000 + 10 });
    const params = { headers: { 'Content-Type': 'application/json' } };

    // Імітація битви ставок
    const res = http.post('http://localhost:5263/api/auctions/123/bids', payload, params);
    check(res, { 'status is 200 or 400': (r) => r.status === 200 || r.status === 400 });
    sleep(0.1); // Швидкі запити
}