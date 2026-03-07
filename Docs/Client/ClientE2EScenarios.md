# Client E2E Scenarios

Date: YYYY-MM-DD  
Purpose: Define end-to-end validation scenarios for client-server flows.

## 1. Common Preconditions
- Server status:
- Data version:
- Test account:
- Client build type (Dev/Release):

## 2. Scenario List
- Scenario A: `Auth -> Enter -> Kingdom`
- Scenario B: `Gacha -> Reward -> Cookie`
- Scenario C: `WorldStage -> Reward -> State Sync`

## 3. Scenario A

### 3.1 Goal
- 

### 3.2 Preconditions
- 

### 3.3 Steps
1. 
2. 
3. 

### 3.4 Expected Results
- Server response:
- Client state change:
- UI change:

### 3.5 Failure Cases
- Invalid session
- Duplicate request
- Timeout

### 3.6 Log Checkpoints
- Request:
- Response:
- Error:

## 4. Scenario B

### 4.1 Goal
- 

### 4.2 Preconditions
- 

### 4.3 Steps
1. 
2. 
3. 

### 4.4 Expected Results
- Server response:
- Client state change:
- UI change:

### 4.5 Failure Cases
- Insufficient currency
- Expired schedule
- Invalid parameter

### 4.6 Log Checkpoints
- Request:
- Response:
- Error:

## 5. Scenario C

### 5.1 Goal
- 

### 5.2 Preconditions
- 

### 5.3 Steps
1. 
2. 
3. 

### 5.4 Expected Results
- Server response:
- Client state change:
- UI change:

### 5.5 Failure Cases
- Duplicate reward claim
- Invalid stage state
- State synchronization mismatch

### 5.6 Log Checkpoints
- Request:
- Response:
- Error:

## 6. Regression Checklist
- [ ] Login and session refresh work correctly
- [ ] Kingdom placement/save flow works correctly
- [ ] Gacha reward application works correctly
- [ ] Cookie growth values are synchronized correctly
- [ ] Duplicate world reward claims are blocked

## 7. Automation Plan
- PlayMode test targets:
- Required mocks/stubs:
- CI execution conditions:

## 8. Change Log
- YYYY-MM-DD: Initial draft
